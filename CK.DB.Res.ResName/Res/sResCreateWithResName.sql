-- SetupConfig: { "Requires": [ "CK.sResNameCreate" ] }
--
-- Creates a resource with an initial ResName.
--
create procedure CK.sResCreateWithResName
(
	@ResName varchar(128),
	@ResId int output
)
as 
begin
	--[beginsp]

	exec CK.sResCreate @ResId output;
	exec CK.sResNameCreate @ResId, @ResName;
	
	--[endsp]
end