CREATE TABLE TestImage (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE,
    FaceLocation_x INTEGER NOT NULL CHECK (FaceLocation_x >= 0),
    FaceLocation_y INTEGER NOT NULL CHECK (FaceLocation_y >= 0),
    FaceLocation_width INTEGER NOT NULL CHECK (FaceLocation_width > 0),
    FaceLocation_height INTEGER NOT NULL CHECK (FaceLocation_height > 0),
    Img BLOB NOT NULL,
    HorizontalHeadRotation INTEGER (1) NOT NULL,
    DateCreated DATETIME NOT NULL,
    FaceEncoding BLOB NOT NULL,
    IsReferenceImg BOOLEAN NOT NULL
);