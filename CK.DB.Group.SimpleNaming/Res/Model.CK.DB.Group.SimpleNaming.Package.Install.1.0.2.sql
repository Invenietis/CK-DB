--[beginscript]

alter table CK.tGroup add  
	GroupName nvarchar(128) collate Latin1_General_100_CI_AI not null constraint DF_CK_tGroup_GroupName default( newid() ),
	constraint UK_CK_tGroup_GroupName unique( GroupName );

--[endscript]
--[beginscript]

update CK.tGroup set GroupName = N'' where GroupId = 0;
update CK.tGroup set GroupName = N'System' where GroupId = 1;
update CK.tGroup set GroupName = N'Administrators' where GroupId = 2;

--[endscript]
