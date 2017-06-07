--[beginscript]
--
-- This table is populated directly by the settle scripts of the packages that
-- implement providers by calling sAuthProviderRegister.
--
create table CK.tAuthProvider
(
	AuthProviderId int not null identity(0,1),
	-- "Google" or "Basic" or "Oidc".
    -- There must not be any dot '.' in the ProviderName.
    -- When IsMultiScheme = 1 (this is the case for Oidc), this provider
    -- can contain more than one actual Client Scheme per user.
	-- Using a CI collation here to avoid ambiguities.
	ProviderName varchar(64) collate Latin1_General_100_CI_AS not null,
	constraint CK_CK_AuthProvider_ProviderName check (CharIndex( '.', ProviderName ) = 0),
	-- Table name with its schema that holds at least UserId and LastLoginTime columns.
	UserProviderSchemaTableName nvarchar(128) not null,
	IsEnabled bit not null, 
    -- This bit indicates that this provider can handle more than one client sheme.
	IsMultiScheme bit not null, 

	constraint PK_CK_AuthProvider primary key( AuthProviderId ),
	constraint UK_CK_AuthProvider_ProviderName unique( ProviderName )
);

insert into CK.tAuthProvider( ProviderName, UserProviderSchemaTableName, IsEnabled, IsMultiScheme ) 
    values( '', N'', 0, 0 );

--[endscript]