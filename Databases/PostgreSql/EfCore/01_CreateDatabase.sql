-- language=PostgreSQL

CREATE SCHEMA example_app;

CREATE TABLE example_app.audit_log
(
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    occured timestamptz  NOT NULL,
    message text NOT NULL
)
