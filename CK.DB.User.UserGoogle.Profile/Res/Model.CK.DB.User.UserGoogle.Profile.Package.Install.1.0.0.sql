--[beginscript]

alter table CK.tUserGoogle add
	FirstName nvarchar( 255 ) collate Latin1_General_100_CI_AS not null constraint DF_TEMP1 default(N''),
	LastName nvarchar( 255 ) collate Latin1_General_100_CI_AS not null constraint DF_TEMP2 default(N''),
	UserName nvarchar( 255 ) collate Latin1_General_100_CI_AS not null constraint DF_TEMP3 default(N''),
	PictureUrl varchar( 255 ) collate Latin1_General_100_BIN2 not null constraint DF_TEMP4 default('');

alter table CK.tUserGoogle drop constraint DF_TEMP1;
alter table CK.tUserGoogle drop constraint DF_TEMP2;
alter table CK.tUserGoogle drop constraint DF_TEMP3;
alter table CK.tUserGoogle drop constraint DF_TEMP4;

--[endscript]
