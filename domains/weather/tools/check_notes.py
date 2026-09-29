"""Validate companion *.notes.json files against the contracts they describe.

Standard library only. Usage: python tools/check_notes.py [root_dir]

Fails (exit 1) if a note's JSON Pointer no longer resolves in its contract.
Reports (without failing) properties that have no note.
"""
import json
import sys
from pathlib import Path


def resolve(doc, pointer):
    """Resolve a '#/a/b' JSON Pointer (RFC 6901); return True if it exists."""
    if not pointer.startswith("#"):
        return False
    node = doc
    for part in [p for p in pointer[1:].split("/") if p != ""]:
        part = part.replace("~1", "/").replace("~0", "~")
        if isinstance(node, dict) and part in node:
            node = node[part]
        elif isinstance(node, list) and part.isdigit() and int(part) < len(node):
            node = node[int(part)]
        else:
            return False
    return True


def property_pointers(node, prefix="#"):
    """Yield pointers to every entry under a 'properties' keyword."""
    if isinstance(node, dict):
        for key, value in node.items():
            if key in ("allOf", "anyOf", "oneOf", "if", "then", "else", "not"):
                continue  # rule branches restate properties; not separate elements
            here = f"{prefix}/{key.replace('~', '~0').replace('/', '~1')}"
            if key == "properties" and isinstance(value, dict):
                for name in value:
                    esc = name.replace("~", "~0").replace("/", "~1")
                    yield f"{here}/{esc}"
            yield from property_pointers(value, here)
    elif isinstance(node, list):
        for i, value in enumerate(node):
            yield from property_pointers(value, f"{prefix}/{i}")


def main():
    root = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(__file__).resolve().parent.parent / "contracts"
    failed = False
    for notes_path in sorted(root.rglob("*.notes.json")):
        notes_doc = json.loads(notes_path.read_text(encoding="utf-8"))
        contract_path = notes_path.parent / notes_doc["contract"]
        if not contract_path.is_file():
            print(f"ERROR {notes_path.relative_to(root)}: contract not found: {notes_doc['contract']}")
            failed = True
            continue
        contract = json.loads(contract_path.read_text(encoding="utf-8"))
        notes = notes_doc.get("notes", {})
        for pointer in notes:
            if not resolve(contract, pointer):
                print(f"ERROR {notes_path.relative_to(root)}: orphaned pointer {pointer}")
                failed = True
        for pointer in property_pointers(contract):
            if pointer not in notes:
                print(f"note  {contract_path.relative_to(root)}: no note for {pointer}")
    if failed:
        sys.exit(1)
    print("OK: all note pointers resolve.")


if __name__ == "__main__":
    main()
