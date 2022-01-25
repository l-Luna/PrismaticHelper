local drawableSpriteStruct = require("structs.drawable_sprite")

local multiLockedDoor = {}

local doorTypes = {
    { id = "wood", sfx = "event:/game/03_resort/key_unlock" },
    { id = "temple", sfx = "event:/game/05_mirror_temple/key_unlock_light" },
    { id = "temple_b", sfx = "event:/game/05_mirror_temple/key_unlock_dark" },
    { id = "moon", sfx = "event:/game/03_resort/key_unlock" }
}

multiLockedDoor.name = "PrismaticHelper/MultiLockedDoor"
multiLockedDoor.placements = {}

for type in doorTypes do
    table.insert(multiLockedDoor.placements, {
        name = "multi_locked_door_" .. type.id,
        data = {
            "door" => "PrismaticHelper/multiLockDoor/base_" .. type.id,
            "lock" => "PrismaticHelper/multiLockDoor/mini_lock",
            "unlockSfx" => type.sfx,
            "keys" => 2
        }
    })
end

multiLockedDoor.fieldInformation = {
    keys = {
        fieldType = "integer"
    }
}

function multiLockedDoor.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local sprites = {}

    local mainSprite = drawableSpriteStruct.fromTexture(entity.door, { x = x, y = y })
    bodySprite:setJustification(0.5, 0.5)
    bodySprite.depth = 1
    table.insert(sprite, mainSprite)

    for i = 1, entity.keys do
        local kx = math.cos(math.pi * 2 * (i / entity.keys)) * 10
        local ky = math.sin(math.pi * 2 * (i / entity.keys)) * 10
        local lockSprite = drawableSpriteStruct.fromTexture(entity.lock, { x = x + kx, y = y + ky })
        lockSprite.depth = 3
        table.insert(sprites, lockSprite)
    end

    return sprites
end

return multiLockedDoor