CREATE TABLE User(
    UserId INTEGER PRIMARY KEY,
    Username TEXT(20) NOT NULL
);

CREATE TABLE TestImage(
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE,
    FaceLocation_x INTEGER NOT NULL CHECK (FaceLocation_x >= 0),
    FaceLocation_y INTEGER NOT NULL CHECK (FaceLocation_y >= 0),
    FaceLocation_width INTEGER NOT NULL CHECK (FaceLocation_width > 0),
    FaceLocation_height INTEGER NOT NULL CHECK (FaceLocation_height > 0),
    Img BLOB NOT NULL,
    HorizontalHeadRotation INTEGER (1) NOT NULL,
    DateCreated DATETIME NOT NULL,
    FaceEncoding BLOB NOT NULL,
    IsReferenceImg BOOLEAN NOT NULL,
    UserId INTEGER NOT NULL,
    FOREIGN KEY(UserId) REFERENCES User(UserId)
);

CREATE TABLE WorkTimeEvent (
    Id INTEGER PRIMARY KEY,
    EventName INT(1) NOT NULL,
    AggregateId BIGINT NOT NULL,
    AggregateVersion BIGINT NOT NULL,
    Date DATETIME NOT NULL,
    Data TEXT NOT NULL,
    UNIQUE (AggregateId,AggregateVersion) 
);


CREATE TABLE WorkTimeIdSequence (
    Id INTEGER PRIMARY KEY AUTOINCREMENT
);


CREATE TABLE AuthData (
    UserId INTEGER UNIQUE NOT NULL,
    Password TEXT NOT NULL,
    FOREIGN KEY(UserId) REFERENCES User(UserId)
);

INSERT INTO User VALUES(1,'test');
INSERT INTO AuthData VALUES(1,'pass');