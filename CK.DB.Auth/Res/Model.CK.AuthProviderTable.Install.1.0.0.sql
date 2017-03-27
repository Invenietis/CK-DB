--[beginscript]
--
-- This table is populated directly by the settle scripts of the packages that
-- implement providers by calling sAuthProviderRegister.
--
create table CK.tAuthProvider
(
	AuthProviderId int not null identity(0,1),
	-- "Google" or "Basic".
	-- Using a CI collation here to avoid ambiguities.
	ProviderName varchar(64) collate Latin1_General_100_CI_AS not null,
	-- Table name with its schema that holds at least UserId and LastLoginTime columns.
	UserProviderSchemaTableName nvarchar(128) not null,
	IsEnabled bit not null, 

	constraint PK_CK_AuthProvider primary key( AuthProviderId ),
	constraint UK_CK_AuthProvider_ProviderName unique( ProviderName )
);

insert into CK.tAuthProvider( ProviderName, UserProviderSchemaTableName, IsEnabled ) values( '', N'', 0 );

--[endscript]