--[beginscript]
--
-- Holds the scopes: these are non null nvarchar(255) without space in them.
-- Collation is strict (EMAIL is not the same scope as email) to honor any potential case
-- sensitivity of authentication provider (the RFC 6749 states that scopes are case sensitive).
-- Note that ScopeId = 0 does not exist: scopes are used as value types from scope sets,
-- the "empty" scope is not relevant.
-- 
create table CK.tAuthScope
(
	ScopeId int not null identity(1,1),
	ScopeName nvarchar(255) collate Latin1_General_100_BIN2 not null,
	constraint PK_CK_AuthScope primary key (ScopeId),
	constraint UK_CK_AUthScope_ScopeName unique (ScopeName),
	constraint CK_CK_AUthScope_ScopeName check (CHARINDEX(N' ',ScopeName) = 0)
);

--[endscript]