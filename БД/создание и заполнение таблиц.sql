create database [$��������������]

--���������
CREATE TABLE UserTypes (
    UserTypeID INT PRIMARY KEY IDENTITY(1,1),
    UserTypeName NVARCHAR(50) NOT NULL
);

--�������
CREATE TABLE Statuses (
    StatusID INT PRIMARY KEY IDENTITY(1,1),
    StatusName NVARCHAR(100) NOT NULL
);

-- ��������
CREATE TABLE Parts (
    RepairPartID INT PRIMARY KEY IDENTITY(1,1),
    RepairPartName NVARCHAR(100) NOT NULL
);

-- ������������
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(15) NOT NULL,
    Login NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(100) NOT NULL,
    UserTypeID INT NOT NULL,
    FOREIGN KEY (UserTypeID) REFERENCES UserTypes(UserTypeID)
);

-- ������
CREATE TABLE Requests (
    RequestID INT PRIMARY KEY IDENTITY(1,1),
    StartDate DATETIME NOT NULL,
    ProblemDescription NVARCHAR(255) NOT NULL,
    RequestStatusID INT NOT NULL,
    CompletionDate DATETIME NULL,
    RepairPartID INT NULL,
    MasterID INT NOT NULL,
    ClientID INT NOT NULL,
    FOREIGN KEY (RequestStatusID) REFERENCES Statuses(StatusID),  
    FOREIGN KEY (RepairPartID) REFERENCES Parts(RepairPartID),   
    FOREIGN KEY (MasterID) REFERENCES Users(UserID),              
    FOREIGN KEY (ClientID) REFERENCES Users(UserID)               
);

-- �����������
CREATE TABLE Comments (
    CommentID INT PRIMARY KEY IDENTITY(1,1),
    Message NVARCHAR(255) NOT NULL,
    MasterID INT NOT NULL,
    RequestID INT NOT NULL,
    FOREIGN KEY (MasterID) REFERENCES Users(UserID),              
    FOREIGN KEY (RequestID) REFERENCES Requests(RequestID)       
);

-- ������
CREATE TABLE Models (
  ModelID INT PRIMARY KEY IDENTITY(1,1),
  orgTechType NVARCHAR(50) NOT NULL,
  orgTechModel NVARCHAR(100) NOT NULL,
  UNIQUE (orgTechType, orgTechModel) 
);

-- ������� ��������
INSERT INTO Statuses (StatusName) VALUES
('����� ������'),
('� �������� �������'),
('������ � ������');

-- ������� ������ � ������� UserTypes
INSERT INTO UserTypes (UserTypeName) VALUES
('��������'),
('��������'),
('��������'),
('������');

-- ������� ������ � ������� Comments 
INSERT INTO Comments (Message, MasterID, RequestID) VALUES
 ('���������...', 2, 2), 
 ('����� �����������!', 3, 3), 
 ('������� �� �� ������ ������!', 3, 4); 

-- ��������� ������� Requests 
INSERT INTO Requests (StartDate, ProblemDescription, RequestStatusID, CompletionDate, RepairPartID, MasterID, ClientID) VALUES
  ('2023-06-06', '�������� ��������', (SELECT StatusID FROM Statuses WHERE StatusName = '� �������� �������'), NULL, NULL, 2, 7),
  ('2023-05-05', '�������� ��������', (SELECT StatusID FROM Statuses WHERE StatusName = '� �������� �������'), NULL, NULL, 3, 7),
  ('2022-07-07', '�� ������� ���� �� ����� ������������', (SELECT StatusID FROM Statuses WHERE StatusName = '������ � ������'), '2023-01-01', NULL, 2, 8),
  ('2023-08-02', '��������� �������� ������ ������ ������', (SELECT StatusID FROM Statuses WHERE StatusName = '����� ������'), NULL, NULL, 1, 8),
  ('2023-08-02', '��������� ����������', (SELECT StatusID FROM Statuses WHERE StatusName = '����� ������'), NULL, NULL, 1, 9),
  ('2023-08-02', '�������� ��������', (SELECT StatusID FROM Statuses WHERE StatusName = '������ � ������'), '2023-08-03', NULL, 2, 7),
  ('2023-07-09', '�����, �� �� ������������', (SELECT StatusID FROM Statuses WHERE StatusName = '������ � ������'), '2023-08-03', 
   (SELECT RepairPartID FROM Parts WHERE RepairPartName = '����� ������ ����������� ������ ������������'), 2, 8); 

-- ���������� ������� Users 
INSERT INTO Users (FullName, Phone, Login, Password, UserTypeID) VALUES
  ('����� ���� ����������', '89210563128', 'login1', 'pass1', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = '��������')),
  ('����� ��������� ���������', '89535078985', 'login2', 'pass2', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = '������')),
  ('��������� ���� ����������', '89210673849', 'login3', 'pass3', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = '������')),
  ('������� ���� ����������', '89990563748', 'login4', 'pass4', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = '��������')),
  ('����� ������ ����������', '89994563847', 'login5', 'pass5', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = '��������')),
  ('��������� ���� ����������', '89219567849', 'login11', 'pass11', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = '��������')),
  ('������� ������� �����', '89219567841', 'login12', 'pass12', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = '��������')),
  ('�������� ���� �����������', '89219567842', 'login13', 'pass13', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = '��������')),
  ('������ ������ �������������', '89219567843', 'login14', 'pass14', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = '��������')),
  ('�������� �������� �������������', '89219567844', 'login15', 'pass15', (SELECT UserTypeID FROM UserTypes WHERE UserTypeName = '������'));

      ALTER TABLE Requests
      ADD ModelID INT; 
      

-- ���������� ������� ModelID � ������� Requests
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Requests') AND name = 'ModelID')
BEGIN
    ALTER TABLE Requests
    ADD ModelID INT;
END;

-- �������� �������� �����
ALTER TABLE Requests
ADD CONSTRAINT FK_Requests_Models FOREIGN KEY (ModelID) REFERENCES Models(ModelID);

--������� ������ � ������
insert into Models values 
('���������','DEXP Aquilon O286'),
('���������','DEXP Atlas H388'),
('�������','MSI GF76 Katana 11UC-879XPU ������'),
('�������','MSI Modern 15 B12M-211RU ������'),
('�������','HP LaserJet Pro M404dn');

--������� Parts ������
