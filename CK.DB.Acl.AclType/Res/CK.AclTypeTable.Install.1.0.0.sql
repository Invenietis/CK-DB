--[beginscript]

create table CK.tAclType
(
	AclTypeId int not null,
	ConstrainedGrantLevel bit not null,
	constraint PK_CK_tAclType primary key clustered ( AclTypeId )
);

-- The 0 AclType is not constrained.     
insert into CK.tAclType( AclTypeId, ConstrainedGrantLevel ) values( 0, 0 );

--[endscript]

--[beginscript]

alter table CK.tAcl add 
	AclTypeId int not null constraint DF_TEMP0 default( 0 ),
	constraint FK_CK_tAcl_AclTypeId foreign key ( AclTypeId ) references CK.tAclType ( AclTypeId );

alter table CK.tAcl drop constraint DF_TEMP0;

--[endscript]
