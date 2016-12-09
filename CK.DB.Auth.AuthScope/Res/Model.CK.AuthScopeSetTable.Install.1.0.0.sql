--[beginscript]
--
-- Defines the scopes set.
-- 
create table CK.tAuthScopeSet
(
	ScopeSetId int not null identity(0,1),
	constraint PK_CK_AuthScopeSet primary key (ScopeSetId)
);

-- The ScopeSet 0 can not be modified. This is done thanks to 
-- the CK_CK_AuthScopeSetContent_ZeroScopeSetId check constraint.
insert into CK.tAuthScopeSet default values;

--[endscript]

--[beginscript]
-- 
-- Each scope of a ScopeSet can be in Waiting, Accepted or Rejected status. 
-- 
create table CK.tAuthScopeSetContent
(
	ScopeSetId int not null,
	ScopeId int not null,
	WARStatus char(1) not null,
	WARStatusLastWrite datetime2(2) not null,
	constraint PK_CK_AuthScopeSetContent primary key (ScopeSetId,ScopeId),
	constraint CK_CK_AuthScopeSetContent_WARStatus check (WARStatus in ('W','A','R')),
	constraint CK_CK_AuthScopeSetContent_ZeroScopeSetId check (ScopeSetId <> 0),
	constraint CK_CK_AuthScopeSetContent_ZeroScopeId check (ScopeId <> 0),
	constraint FK_CK_AuthScopeSetContent_ScopeSetId foreign key( ScopeSetId ) references CK.tAuthScopeSet( ScopeSetId ),
	constraint FK_CK_AuthScopeSetContent_ScopeId foreign key( ScopeId ) references CK.tAuthScope( ScopeId )
);

--[endscript]