CREATE TABLE WorkTimeEvent (
    Id INTEGER PRIMARY KEY,
    AggregateId INTEGER NOT NULL,
    EventName INT(1) NOT NULL,
    Date DATETIME NOT NULL,
    Data TEXT NOT NULL,
    AggregateVersion BIGINT NOT NULL
);