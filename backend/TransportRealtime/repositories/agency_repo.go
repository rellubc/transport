package repositories

import (
	models "TransportRealtime/models/static"
	"context"

	"github.com/jackc/pgx/v5/pgxpool"
)

type AgencyRepository struct {
	DB *pgxpool.Pool
}

func NewAgencyRepository(db *pgxpool.Pool) *AgencyRepository {
	return &AgencyRepository{DB: db}
}

func (r *AgencyRepository) GetAgencies() ([]models.Agency, error) {
	rows, err := r.DB.Query(context.Background(), "SELECT * FROM agencies")
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var agencies []models.Agency
	for rows.Next() {
		var a models.Agency
		err := rows.Scan(
			&a.AgencyId,
			&a.AgencyName,
			&a.AgencyUrl,
			&a.AgencyTimezone,
			&a.AgencyLanguage,
			&a.AgencyPhone,
			&a.AgencyFareUrl,
			&a.AgencyEmail,
		)

		if err != nil {
			return nil, err
		}

		agencies = append(agencies, a)
	}

	return agencies, nil
}

func (r *AgencyRepository) GetAgency(agencyId string) (models.Agency, error) {
	row := r.DB.QueryRow(context.Background(), "SELECT * FROM agencies WHERE agency_id = $1", agencyId)

	var a models.Agency
	err := row.Scan(
		&a.AgencyId,
		&a.AgencyName,
		&a.AgencyUrl,
		&a.AgencyTimezone,
		&a.AgencyLanguage,
		&a.AgencyPhone,
		&a.AgencyFareUrl,
		&a.AgencyEmail,
	)

	return a, err
}
