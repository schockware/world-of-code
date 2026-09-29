// Package vectors loads the conformance vectors that every nation runs.
package vectors

import (
	"encoding/json"
	"os"
	"path/filepath"
	"sort"
)

// Vector is one conformance vector.
type Vector struct {
	ID           string          `json:"id"`
	Requirements []string        `json:"requirements"`
	Operation    string          `json:"operation"`
	Input        json.RawMessage `json:"input"`
	Expect       json.RawMessage `json:"expect"`
}

// File is one area's vectors.
type File struct {
	Name    string
	Vectors []Vector
}

// Load reads every *.json file in dir, in name order.
func Load(dir string) ([]File, error) {
	paths, err := filepath.Glob(filepath.Join(dir, "*.json"))
	if err != nil {
		return nil, err
	}
	sort.Strings(paths)

	files := make([]File, 0, len(paths))
	for _, path := range paths {
		data, err := os.ReadFile(path)
		if err != nil {
			return nil, err
		}
		var doc struct {
			Vectors []Vector `json:"vectors"`
		}
		if err := json.Unmarshal(data, &doc); err != nil {
			return nil, err
		}
		files = append(files, File{Name: filepath.Base(path), Vectors: doc.Vectors})
	}
	return files, nil
}
