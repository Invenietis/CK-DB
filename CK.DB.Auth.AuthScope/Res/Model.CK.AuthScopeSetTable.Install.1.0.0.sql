--[beginscript]
--
-- Defines the scopes set.
-- 
create table CK.tAuthScopeSet
(
	ScopeSetId int not null identity(0,1),
	constraint PK_CK_AuthScopeSet primary key (ScopeSetId)
);

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
	constraint CK_CK_AuthScopeSetContent_WARStatus check (WARStatus in ('W','A','R'))
);

--[endscript]