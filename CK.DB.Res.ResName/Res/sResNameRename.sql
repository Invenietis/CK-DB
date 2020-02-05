-- SetupConfig: {}
--
-- Renames a resource by its resource identifier.
--
create procedure CK.sResNameRename
(
    @ResId int,
    @NewName varchar(128),
	@WithChildren bit = 1
)
as 
begin
	if @ResId <= 1 throw 50000, 'Res.NoRename', 1;
	
	-- This is to not break existing code. 
	-- 8.0.0 version has no more this fragment sinc it is the sResNameRenameResName that has it.

	--<PreRename revert />

	declare @OldName varchar(128);
	select @OldName = ResName from CK.tResName where ResId = @ResId;

	if @OldName is not null 
	begin
		exec CK.sResNameRenameResName @OldName, @NewName, @WithChildren;
	end

	--<PostRename />

end
