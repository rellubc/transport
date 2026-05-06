package handlers

import (
	"TransportRealtime/repositories"
	"encoding/json"
	"net/http"

	"github.com/go-chi/chi/v5"
)

func GetNotesHandler(repo *repositories.NoteRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		notes, err := repo.GetNotes()
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(notes)
	}
}

func GetNoteHandler(repo *repositories.NoteRepository) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		noteId := chi.URLParam(r, "note_id")

		note, err := repo.GetNote(noteId)
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}

		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(note)
	}
}
