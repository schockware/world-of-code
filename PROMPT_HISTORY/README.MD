*Authored by Claude Sonnet 5.5 (Anthropic), with Steven Chock as co-author.*

# Prompt history

How implementer actors (chats that take specifications and write code) record what they were asked and what they did. The rules an actor follows are in `CLAUDE.md`; this file holds the definitions, disclaimers, and reasons behind them.

## Files

| File | Committed | Contains |
|---|---|---|
| `PROMPT_HISTORY.LOG` | yes | Summary prompt, redaction check result, actions, token tally |
| `PROMPT_HISTORY.REDACTED` | no (gitignored) | The original prompt with sensitive content replaced by `[REDACTED:{category}]` |
| `PROMPT_HISTORY.AUDIT` | no (gitignored) | One line per entry whose redaction needs supplemental review (categories only, never the content) |
| `PROMPT_HISTORY.AUDIT.EXAMPLE` | yes | A sample audit line from the faux redaction test, invented values only; actors do not write to it |
| `PROMPT_HISTORY_README.MD` | yes | This file |

Both logs are newest-first. The two files share an entry heading (`## {date time} | {model}`), so an entry in one can be matched to its entry in the other.

## Definitions

- **Implementer actor:** a chat that takes specs and performs code (designs, implementations, conformance runs). Spec authoring is not implementing, but a chat that does both logs its prompts anyway.
- **Summary prompt:** a short restatement of the request in the actor's own words. It exists so the committed log is useful without carrying the original text.
- **Audit line:** a one-line flag that an entry needs a human to look at its redaction.
- **Redaction check:** the actor's statement that it looked for sensitive content, and what it removed, by category.
- **Derived tokens:** the drop in the visible "tokens left" counter across the prompt.
- **Estimated tokens:** a rough figure from prompt and reply size, used when no counter is visible.
- **No code changes:** the action recorded when a prompt changed no files (questions, discussion, review).

## Token tally

- Every figure carries a `~` and is rounded to the nearest 100.
- The counter has been seen to move unpredictably (including rising between turns), so even derived figures are approximate.
- The tally excludes the cost of writing the log entry itself.
- Subagent usage is included only when reported.
- These numbers are not billing figures. Check the provider's own metering for cost.

## What is not logged

Prompts whose purpose is to change the logging process itself (the log, redacted, and audit files, their rules in `CLAUDE.md`, and this README) are not logged. Everything else is, including questions.

## Redaction

- **Redacted:** secrets, credentials, API keys, tokens, identifiers (UUIDs, account IDs, and similar), personal data (names beyond the project's authors, emails, addresses, phone numbers), and pasted third-party content.
- **Marker:** `[REDACTED:{category}]`, for example `[REDACTED:api-key]`.
- **Two layers, because neither is bulletproof:**
  1. The committed log carries only a summary, so a missed secret in the original does not reach it.
  2. The original lives only in the gitignored `.REDACTED` file, so a missed secret there does not reach the repository either.
- **Limits:** gitignore stops commits, not copies. The `.REDACTED` file is still readable on disk, in backups, and by tools that ignore `.gitignore`. A summary can still leak if the actor paraphrases a secret. If a secret is found in either file, delete it from the file, rotate the secret, and note the removal in the next entry.
- **Audit trail:** any entry with a redaction, or where the actor is unsure, gets a line in `PROMPT_HISTORY.AUDIT`, newest first: `{date time} | {model} | {categories} | {why review is needed} | OPEN`. The line matches the entry by its time. A reviewer closes it by prepending `{date time} | REVIEWED {original entry time} | {outcome}`, because older lines are never edited. The audit file is gitignored and so local to one checkout, but a line must still name only the category and reason, never the secret.
- **Example record:** `PROMPT_HISTORY.AUDIT.EXAMPLE` is committed and holds the `2026-10-02 08:32` faux redaction test line, to show what an audit line looks like when a Claude model writes one. It uses invented values. The real `PROMPT_HISTORY.AUDIT` stays gitignored, so real review flags never reach the repository.
- Before writing to the `.REDACTED` or `.AUDIT` file, the actor confirms `.gitignore` lists it.

## Why secrets never belong in a prompt

Redaction here limits what lands in the repository. It does nothing about where else a prompt travels. Do not put real secrets, credentials, or identifiers in any chat prompt, because the prompt is copied to more places than this repository:

- **Transcripts:** the chat service and the local tooling keep conversation transcripts. Anyone who gets access to the account, a shared link, an exported chat, or a synced device can read them. Deleting a message here does not delete those copies.
- **Local session files:** the editor and CLI store sessions and memory on disk, in plaintext, outside the repository and outside `.gitignore`. Backups and cloud sync carry them along.
- **Tool traffic:** a secret in a prompt can be passed into tool calls, shell commands, subagent prompts, and command output, and from there into shell history, logs, and error messages.
- **Reuse and echo:** a model can repeat a secret in later replies, files, commit messages, or code comments. The redaction check is meant to catch that, but it is a check by the same actor that saw the secret and it can miss.
- **Prompt injection:** content an actor reads (files, web pages, tool output) can carry instructions that try to extract anything in its context. A secret already in the conversation is in reach of that attack.
- **Shoulder and screen exposure:** prompts show in terminals, IDE panels, screen shares, and screenshots.
- **Third parties:** connected tools and servers (MCP servers, connectors) may receive parts of the conversation, under their own retention rules.

What to do instead:

- Give the actor a placeholder, a fictional value, or the name of an environment variable or secret store, never the value.
- Keep real secrets in environment variables, a secrets manager, or untracked config that the actor does not read.
- If a real secret is pasted by mistake, treat it as leaked: rotate it first, then remove it from the log files as described under Redaction.
- The Faux Redaction test entry in the log uses invented values for exactly this reason.

## Known gaps

- The `.REDACTED` and `.AUDIT` files are local to one checkout, so original prompts and review flags are not shared between machines.
- Timestamps use the machine's local time and carry no time zone.
- Entries written before these rules (the first entry in the log) use the earlier format with a verbatim prompt and are left as written.

# Developer Comments
Make sure your codebase does not have secrets or sensitive information. Models that read your files won't always notice they've pulled a secret into their context, which can expose it via other means. 

I highly recommend Environment Variables, User Secrets, and Secure Configuration Services.

## Appendix A: Using secrets with AI models (reference material)

Researched 2026-10-02. Links were current then, and the Claude Code settings in particular change between versions. Items marked "primary" come from the publisher of the tool or standard. Check the source before relying on a detail.

### Principles

1. **Keep secrets out of the model's context.** A secret the model never reads cannot be echoed, logged, or extracted. Prefer a value that reaches the program through the environment, not through the chat.
2. **Give the model the name, not the value.** Refer to `DATABASE_URL` or "the key in the secrets manager". Let the program read the value at run time.
3. **Block reads, don't just ask.** A rule that denies access is stronger than an instruction that says not to look.
4. **Use separate, scoped, short-lived credentials** for development, testing, and production, so one leak has limited reach.
5. **Assume leaks happen.** Scan for them, rotate on suspicion, and keep an audit trail.

### Controlling what Claude Code can read (primary)

- **`Read` deny rules.** Add paths such as `Read(./.env)` or `Read(./secrets/**)` to `permissions.deny` in settings. A bare name like `Read(.env)` matches at any depth. On Windows, `//**/.env` matches `.env` files on any drive. A `Read` deny also blocks the Edit and Write tools on the same path. Deny rules apply to the symlink target as well. A `.claudeignore` file has no effect; move its entries into `Read` deny rules.
- **Deny rules match command text.** A deny on the file tools does not by itself stop a shell command from printing a file. For enforcement that does not depend on the command text, use sandboxing (filesystem and network isolation for Bash) or a PreToolUse hook.
- **Network commands.** `curl`, `wget`, and similar commands are not auto-approved by default. Deny them, or use the sandbox network allowlist, if a prompt-injected command must not send data out.
- **Isolation.** Dev containers or VMs add a boundary for untrusted content or sensitive repositories. The security page recommends reviewing proposed commands, avoiding piping untrusted content to Claude, and auditing settings with `/permissions`.
- **Credential storage.** Claude Code's own tokens go to the macOS Keychain when available. On Windows they are stored in a file that inherits the access controls of your user profile directory.
- **Connectors and MCP servers.** Project-scoped servers come from `.mcp.json`, but other scopes, plugins, and connectors are configured outside the repository. Anthropic does not security-audit MCP servers, so use ones you trust.
- **Limit of the guidance.** The security page states that no system is completely immune to attack.

### Handling API keys (primary)

Anthropic's API key guidance: never share a key; store it in environment variables or encrypted secrets, never in code or config files; use separate keys per purpose and environment; rotate on a schedule (90 days is the example given); set spend limits; scan repositories for committed secrets with a tool such as Gitleaks; use a key management system for access control, audit trails, and rotation; and revoke a compromised key immediately.

### Storing secrets for the program itself

- **Environment variables.** They keep secrets out of code, but they are plain text, and anyone who compromises the machine or process can read them. A model with shell access can print them, so pair them with the read and sandbox controls above.
- **.NET user secrets (development only).** `dotnet user-secrets init` stores values in a JSON file in the user profile, outside the project tree. The values are not encrypted and the store is not trusted. Use a managed service such as Azure Key Vault in production.
- **Secrets managers and key vaults.** Centralize storage, scope access, rotate, revoke, and audit. OWASP's cheat sheet covers the full lifecycle: creation, storage, access control, rotation, revocation, expiration, and auditing.
- **Operating-system keychains.** Fetch the secret into the process environment at launch, so it never sits in a file in the repository.

### Catching leaks

- **Push protection and secret scanning.** GitHub can block pushes, commits, uploads, and API requests that contain supported secret patterns, and raises an alert when a contributor bypasses the block.
- **Repository scanning.** Scan history as well as the working tree, because a secret removed in a later commit remains in history.
- **Rotation is the fix.** Deleting a leaked secret does not make it safe. Revoke or rotate it first.

### Threat model references

- OWASP Top 10 for LLM Applications: LLM01 Prompt Injection (direct, or indirect through files, web pages, and documents the model reads), LLM02 Sensitive Information Disclosure, and Excessive Agency, which matters when a model can run tools with your credentials.

### Sources

- Claude Code security (primary): <https://code.claude.com/docs/en/security>
- Claude Code permissions, including `Read` deny rules (primary): <https://code.claude.com/docs/en/permissions>
- Anthropic, API key best practices (primary): <https://support.claude.com/en/articles/9767949-api-key-best-practices-keeping-your-keys-safe-and-secure>
- OWASP Secrets Management Cheat Sheet (primary): <https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html>
- OWASP Top 10 for LLM Applications (secondary summaries read): <https://www.promptfoo.dev/blog/owasp-top-10-llms-tldr/>
- Microsoft, safe storage of app secrets in ASP.NET Core development (primary): <https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets>
- GitHub, push protection (primary): <https://docs.github.com/en/code-security/secret-scanning/introduction/about-push-protection>
