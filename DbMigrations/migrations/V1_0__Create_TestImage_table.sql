CREATE TABLE TestImage (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE,
    FaceLocation_x INTEGER NOT NULL CHECK (FaceLocation_x >= 0),
    FaceLocation_y INTEGER NOT NULL CHECK (FaceLocation_y >= 0),
    FaceLocation_right INTEGER NOT NULL CHECK (FaceLocation_right > 0),
    FaceLocation_bottom INTEGER NOT NULL CHECK (FaceLocation_bottom > 0),
    Img BLOB NOT NULL,
    Rotation INTEGER (1) NOT NULL,
    DateCreated DATETIME NOT NULL,
    FaceEncoding BLOB NOT NULL,
    IsReferenceImg BOOLEAN NOT NULL
);