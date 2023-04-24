local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local customHangingLamp = {}

customHangingLamp.name = "PrismaticHelper/CustomHangingLamp"
customHangingLamp.depth = 2000
customHangingLamp.minimumSize = {0, 16}
customHangingLamp.canResize = {false, true}
customHangingLamp.placements = {
    name = "hanging_lamp",
    data = {
        height = 16,
        attached = false,
        sprite = "",
        --glowColour = "ffffff",
        glowStartRadius = 24,
        glowEndRadius = 48
    }
}

customHangingLamp.fieldInformation = {
    glowColour = {
        fieldType = "color"
    }
}

-- Manual offsets and justifications of the sprites
function customHangingLamp.sprite(room, entity)
    local sprites = {}
    local height = math.max(entity.height or 0, 16)
    local texture = (entity.sprite and string.len(entity.sprite) > 0 and entity.sprite) or "objects/hanginglamp"

    local topSprite = drawableSprite.fromTexture(texture, entity)

    topSprite:setJustification(0, 0)
    topSprite:setOffset(0, 0)
    topSprite:useRelativeQuad(0, 0, 8, 8)

    table.insert(sprites, topSprite)

    for i = 0, height - 16, 8 do
        local middleSprite = drawableSprite.fromTexture(texture, entity)

        middleSprite:setJustification(0, 0)
        middleSprite:setOffset(0, 0)
        middleSprite:addPosition(0, i)
        middleSprite:useRelativeQuad(0, 8, 8, 8)

        table.insert(sprites, middleSprite)
    end

    local bottomSprite = drawableSprite.fromTexture(texture, entity)

    bottomSprite:setJustification(0, 0)
    bottomSprite:setOffset(0, 0)
    bottomSprite:addPosition(0, height - 8)
    bottomSprite:useRelativeQuad(0, 16, 8, 8)

    table.insert(sprites, bottomSprite)

    return sprites
end

function customHangingLamp.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, 8, math.max(entity.height, 16))
end

return customHangingLamp