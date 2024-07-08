package main

import "time"

type WeatherForecast struct {
	Date         CustomDate `json:"date"`
	TemperatureC int        `json:"temperatureC"`
	TemperatureF int        `json:"temperatureF"`
	Summary      string     `json:"summary"`
}

// CustomDate wraps time.Time to provide custom unmarshalling
type CustomDate struct {
	time.Time
}

// UnmarshalJSON customizes the JSON unmarshalling for CustomDate
func (cd *CustomDate) UnmarshalJSON(b []byte) error {
	// The date format in the JSON
	const layout = `"2006-01-02"`
	t, err := time.Parse(layout, string(b))
	if err != nil {
		return err
	}
	cd.Time = t
	return nil
}
