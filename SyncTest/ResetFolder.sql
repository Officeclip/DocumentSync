DELETE FROM dmmasters
DELETE FROM dmgroupfolders

DECLARE @folderIdentity int

INSERT INTO dmgroupfolders (name, parent_id, parent_path) VALUES ('DbFolder', NULL, '');
SET @folderIdentity = @@IDENTITY

INSERT INTO dmmasters (name, folder_id) VALUES ('File1.txt', @folderIdentity);

INSERT INTO dmgroupfolders (name, parent_id, parent_path) VALUES ('Folder1', @folderIdentity, 'DbFolder');
SET @folderIdentity = @@IDENTITY

INSERT INTO dmmasters (name, folder_id) VALUES ('File2.txt', @folderIdentity);

INSERT INTO dmgroupfolders (name, parent_id, parent_path) VALUES ('Folder2', @folderIdentity, 'DbFolder/Folder1');
SET @folderIdentity = @@IDENTITY

INSERT INTO dmmasters (name, folder_id) VALUES ('File3.txt', @folderIdentity);

update dmmasters set last_updated = '2017-01-01 12:00:00.00'
update dmgroupfolders set last_updated = '2017-01-01 12:00:00.00'
