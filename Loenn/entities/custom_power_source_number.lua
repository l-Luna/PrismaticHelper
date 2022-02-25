local drawableSprite = require("structs.drawable_sprite")

local powerSourceNum = {}

powerSourceNum.name = "PrismaticHelper/CustomPowerSourceNumber"
powerSourceNum.depth = -10010
powerSourceNum.placements = {
    name = "custom_power_source_number",
    data = {
        cond = "1:1",
        base = "scenery/powersource_numbers/1",
        glow = "scenery/powersource_numbers/1_glow",
        requiresLightningDisabled = true
    }
}

function powerSourceNum.sprite(room, entity)
    local baseSprite = drawableSprite.fromTexture(entity.base, entity)
    local glowSprite = drawableSprite.fromTexture(entity.glow, entity)
    baseSprite:addPosition(12, 12)
    glowSprite:addPosition(12, 12)

    local sprites = {
        baseSprite,
        glowSprite
    }

    return sprites
end

return powerSourceNum