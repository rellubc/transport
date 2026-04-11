package repositories

import (
	models "TransportRealtime/models/static"
	"context"

	"github.com/jackc/pgx/v5/pgxpool"
)

type CalendarRepository struct {
	DB *pgxpool.Pool
}

func NewCalendarRepository(db *pgxpool.Pool) *CalendarRepository {
	return &CalendarRepository{DB: db}
}

func (r *CalendarRepository) GetCalendars() ([]models.Calendar, error) {
	rows, err := r.DB.Query(context.Background(), "SELECT * FROM calendars")
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var calendars []models.Calendar
	for rows.Next() {
		var calendar models.Calendar
		err := rows.Scan(
			&calendar.ServiceId,
			&calendar.Monday,
			&calendar.Tuesday,
			&calendar.Wednesday,
			&calendar.Thursday,
			&calendar.Friday,
			&calendar.Saturday,
			&calendar.Sunday,
			&calendar.StartDate,
			&calendar.EndDate,
		)

		if err != nil {
			return nil, err
		}

		calendars = append(calendars, calendar)
	}

	return calendars, nil
}

func (r *CalendarRepository) GetCalendar(serviceId string) (models.Calendar, error) {
	row := r.DB.QueryRow(context.Background(), "SELECT * FROM calendars WHERE service_id = $1", serviceId)

	var calendar models.Calendar
	err := row.Scan(
		&calendar.ServiceId,
		&calendar.Monday,
		&calendar.Tuesday,
		&calendar.Wednesday,
		&calendar.Thursday,
		&calendar.Friday,
		&calendar.Saturday,
		&calendar.Sunday,
		&calendar.StartDate,
		&calendar.EndDate,
	)

	return calendar, err
}
