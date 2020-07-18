-- SetupConfig: {}
--
-- Renames a resource by its resource name.
--
create procedure CK.sResNameRenameResName
(
    @OldName varchar(128),
    @NewName varchar(128),
	@WithChildren bit = 1
)
as 
begin
	declare @LenPrefix int;
	set @NewName = RTrim( LTrim(@NewName) );
	set @LenPrefix = len(@OldName)+1;
	if @LenPrefix is null or @LenPrefix = 0 throw 50000, 'Res.RootRename', 1;

	--[beginsp]

	--<PreRename revert />

	if @WithChildren = 1
	begin
		-- Updates child names first.
		update CK.tResName set ResName = @NewName + substring( ResName, @LenPrefix, 128 )
			where ResName like @OldName + '.%';
	end
	-- Updates the resource itself.
	update CK.tResName set ResName = @NewName where ResName = @OldName;

	--<PostRename />
	
	--[endsp]
end
