package repositories

import (
	models "TransportRealtime/models/static"
	"context"

	"github.com/jackc/pgx/v5/pgxpool"
)

type NoteRepository struct {
	DB *pgxpool.Pool
}

func NewNoteRepository(db *pgxpool.Pool) *NoteRepository {
	return &NoteRepository{DB: db}
}

func (r *NoteRepository) GetNotes() ([]models.Note, error) {
	rows, err := r.DB.Query(context.Background(), "SELECT * FROM notes")
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var notes []models.Note
	for rows.Next() {
		var note models.Note
		err := rows.Scan(
			&note.NoteId,
			&note.NoteText,
		)

		if err != nil {
			return nil, err
		}

		notes = append(notes, note)
	}

	return notes, nil
}

func (r *NoteRepository) GetNote(noteId string) (models.Note, error) {
	row := r.DB.QueryRow(context.Background(), "SELECT * FROM notes WHERE note_id = $1", noteId)

	var note models.Note
	err := row.Scan(
		&note.NoteId,
		&note.NoteText,
	)

	return note, err
}
