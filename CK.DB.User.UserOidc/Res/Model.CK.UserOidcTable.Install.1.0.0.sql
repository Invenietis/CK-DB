--[beginscript]

create table CK.tUserOidc
(
	UserId int not null,
    
    -- SchemeSuffix must not contain the "Oidc" provider name prefix nor starts with a dot.
	SchemeSuffix varchar(64) collate Latin1_General_100_CI_AS not null,
	constraint CK_CK_UserOidc_SchemeSuffix check (left(SchemeSuffix,4) <> 'Oidc' and  left(SchemeSuffix,1) <> '.'),
	Scheme as case len(SchemeSuffix) when 0 then 'Oidc' else 'Oidc.' + SchemeSuffix end,

    -- Sub must not be an empty string.
	Sub nvarchar(64) collate Latin1_General_100_BIN2 not null,

	LastLoginTime datetime2(2) not null,
	constraint PK_CK_UserOidc primary key (UserId, SchemeSuffix),
	constraint FK_CK_UserOidc_UserId foreign key (UserId) references CK.tUser(UserId),
	constraint UK_CK_UserOidc_OidcSub unique( SchemeSuffix, Sub )
);

insert into CK.tUserOidc( UserId, SchemeSuffix, Sub, LastLoginTime ) 
	values( 0, '', N'', sysutcdatetime() );

alter table CK.tUserOidc with nocheck add constraint CK_CK_UserOidc_Sub check (len(Sub) > 0);


--[endscript]