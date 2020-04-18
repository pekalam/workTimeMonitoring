CREATE TABLE WorkTimeEvent (
    Id INTEGER PRIMARY KEY,
    EventName INT(1) NOT NULL,
    AggregateId INTEGER NOT NULL,
    AggregateVersion BIGINT NOT NULL,
    Date DATETIME NOT NULL,
    Data TEXT NOT NULL,
    UNIQUE(AggregateId, AggregateVersion)
);


CREATE TABLE WorkTimeIdSequence (
    Id INTEGER PRIMARY KEY AUTOINCREMENT
)