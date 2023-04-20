local drawableNinePatch = require("structs.drawable_nine_patch")

local heater = {}

heater.name = "PrismaticHelper/Heater"
heater.fillColor = {0.7, 0.2, 0.1, 0.6}
heater.borderColor = {0.7, 0.2, 0.1, 0.4}
heater.placements = {
    name = "heater",
    data = {
        width = 8,
        height = 8,
        maxTime = 7
    }
}

local heaterTexture = "PrismaticHelper/heater/boilerplate"
local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

function heater.sprite(room, entity)
    local x, y, width, height = entity.x or 0, entity.y or 0, entity.width or 0, entity.height or 0
    
    local ninePatch = drawableNinePatch.fromTexture(heaterTexture, ninePatchOptions, x, y, width, height)
    
    return ninePatch:getDrawableSprite()
end

local freezer = {}

freezer.name = "PrismaticHelper/Freezer"
freezer.fillColor = {0.1, 0.2, 0.7, 0.6}
freezer.borderColor = {0.1, 0.2, 0.7, 0.4}
freezer.placements = {
    name = "freezer",
    data = {
        width = 8,
        height = 8,
        maxTime = 7
    }
}

return { heater, freezer }