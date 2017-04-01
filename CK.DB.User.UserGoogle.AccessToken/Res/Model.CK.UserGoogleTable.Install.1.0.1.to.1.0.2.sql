
alter table CK.tUserGoogle drop constraint UK_CK_UserGoogle_GoogleAccountId;

alter table CK.tUserGoogle
	alter column GoogleAccountId varchar(36) collate Latin1_General_100_BIN2 not null;

alter table CK.tUserGoogle add constraint UK_CK_UserGoogle_GoogleAccountId unique( GoogleAccountId );
