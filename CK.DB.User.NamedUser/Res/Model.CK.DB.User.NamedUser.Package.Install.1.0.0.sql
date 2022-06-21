--[beginscript]


alter table CK.tUser add FirstName nvarchar(255)
collate Latin1_General_CI_AI not null constraint DF_CK_tUser_FirstName default( N'' )

alter table CK.tUser add LastName nvarchar(255)
collate Latin1_General_CI_AI not null constraint DF_CK_tUser_LastName default( N'' );

--[endscript]
