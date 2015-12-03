
create table CK.tGroup
(
  GroupId int not null,
  CreationDate datetime2(0) not null constraint DF_CK_tGroup_CreationDate default( sysutcdatetime() ),

  constraint PK_CK_tGroup primary key clustered( GroupId ),
  constraint FK_CK_tGroup_ActorId foreign key ( GroupId ) references CK.tActor( ActorId ),
);
--
insert into CK.tGroup( GroupId ) values( 0 );
insert into CK.tGroup( GroupId ) values( 1 );

