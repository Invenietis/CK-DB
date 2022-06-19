-- SetupConfig: {}
--
-- Updates a LastName or FirstName of a User.

create procedure CK.sNamedUserEdit
    @ActorId int, 
    @UserId int, 
    @FirstName nvarchar(255),
    @LastName nvarchar(255)
as 
begin 
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

    --[beginsp]
    declare @OldFirstName nvarchar(255);
    declare @OldLastName nvarchar(255);

    select @OldFirstName = FirstName, @OldLastName = LastName
    from CK.tUser
    where UserId = @UserId;

    if @FirstName is null set @FirstName = @OldFirstName;
    if @LastName is null set @LastName = @OldLastName;

    update CK.tUser
        set FirstName = @FirstName, 
            LastName = @LastName
        where UserId = @UserId
    --[endsp]
end
