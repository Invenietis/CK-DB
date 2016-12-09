--[beginscript]

alter table CK.tUserGoogle add
	ScopeSetId int not null constraint DF_TEMP default(0);
	 
alter table CK.tUserGoogle add
	constraint FK_CK_UserGoogle_ScopeSetId foreign key (ScopeSetId) references CK.tAuthScopeSet(ScopeSetId),
	constraint UK_CK_UserGoogle_ScopeSetId unique( ScopeSetId )

alter table CK.tUserGoogle drop constraint DF_TEMP;

-- ScopeSetId are let to 0 here:
-- The Anonymous holds the default scopes: it is created in by Settle and every 
-- already created UserGoogle is associated to an independent scope set.

--[endscript]