--[beginscript]

alter table CK.tZone 
	add 
		LeftNumber int null,
		RightNumber int null,
		Depth smallint null;

--[endscript]

--[beginscript]

update z set LeftNumber = exist.R*2+1, RightNumber = exist.R*2+2, Depth = 0
	from CK.tZone z
	inner join (select ZoneId, R = (RANK() over(order by ZoneId)-1) from CK.tZone where ZoneId > 0) exist on exist.ZoneId = z.ZoneId
	where z.ZoneId > 0;
update z set LeftNumber = 0, RightNumber = (select max(RightNumber)+1 from CK.tZone), Depth = 0
	from CK.tZone z
	where z.ZoneId = 0; 

alter table CK.tZone alter column LeftNumber int not null;
alter table CK.tZone alter column RightNumber int not null;
alter table CK.tZone alter column Depth int not null;
alter table CK.tZone add 
    constraint CK_CK_Zone_LeftRightNumbers check( LeftNumber < RightNumber ),
    constraint CK_CK_Zone_Depth check( Depth >= 0 );

alter table CK.tZone add 
    constraint UK_CK_Zone_LeftNumber unique( LeftNumber ),
    constraint UK_CK_Zone_RightNumber unique( RightNumber );

exec CKCore.sInvariantRegister 'Zone.DuplicateLeftRightNumbers', N'
	from CK.tZone z
	where	z.LeftNumber in ( select RightNumber from CK.tZone where ZoneId != z.ZoneId 
							  union all
							  select LeftNumber from CK.tZone where ZoneId != z.ZoneId)
			or
			z.RightNumber in ( select RightNumber from CK.tZone where ZoneId != z.ZoneId 
							   union all
							   select LeftNumber from CK.tZone where ZoneId != z.ZoneId)
';
exec CKCore.sInvariantRegister 'Zone.InvalidMaxOrMinNumbers', N'
	from CK.tZone
	where  -- ZoneId=0 is the father of all zones (it starts at 0 and contains all the children).
		  (select max(RightNumber) from CK.tZone) <> (select RightNumber from CK.tZone where ZoneId = 0)
			or
		  (select min(LeftNumber) from CK.tZone where ZoneId = 0) <> 0
			or
		  (select min(LeftNumber) from CK.tZone) <> 0
			or
		  -- Numbering is compact.
		  (select max(RightNumber) from CK.tZone)+1 <> (select 2*count(0) from CK.tZone)
';

--[endscript]
