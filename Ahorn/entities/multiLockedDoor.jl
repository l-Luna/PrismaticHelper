module PrismaticHelperMultiLockedDoor
using ..Ahorn, Maple

@mapdef Entity "PrismaticHelper/MultiLockedDoor" MultiLockedDoor(x::Integer, y::Integer, 
   door::String="", lock::String="", unlockSfx::String="", keys::Integer=1)

const placements = Ahorn.PlacementDict()

doorTypes = Dict{String, @NamedTuple{name::String, sfx::String}}(
    "wood" => (name = "Wood", sfx = "event:/game/03_resort/key_unlock"),
    "temple" => (name = "Temple", sfx = "event:/game/05_mirror_temple/key_unlock_light"),
    "temple_b" => (name = "Temple B", sfx = "event:/game/05_mirror_temple/key_unlock_dark"),
    "moon" => (name = "Moon", sfx = "event:/game/03_resort/key_unlock")
)

for (variant, data) in doorTypes
    key = "Multi-locked Door ($(data.name)) (Prismatic Helper)"
    placements[key] = Ahorn.EntityPlacement(
      MultiLockedDoor,
      "point",
      Dict{String, Any}(
         "door" => "PrismaticHelper/multiLockDoor/base_$(variant)",
         "lock" => "PrismaticHelper/multiLockDoor/mini_lock",
         "unlockSfx" => data.sfx
      )
   )
end

function Ahorn.selection(entity::MultiLockedDoor)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 16, y - 16, 32, 32)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MultiLockedDoor, room::Maple.Room)
    Ahorn.drawSprite(ctx, entity.door, 0, 0)
end

end