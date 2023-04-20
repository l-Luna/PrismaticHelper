local drawableNinePatch = require("structs.drawable_nine_patch")

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

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

local heaterTexture = "PrismaticHelper/temperatureBlocks/boilerplate"

function heater.sprite(room, entity)
    local x, y, width, height = entity.x or 0, entity.y or 0, entity.width or 0, entity.height or 0
    
    local ninePatch = drawableNinePatch.fromTexture(heaterTexture, ninePatchOptions, x, y, width, height)
    
    return ninePatch:getDrawableSprite()
end

local freezer = {}

freezer.name = "PrismaticHelper/Freezer"
freezer.placements = {
    name = "freezer",
    data = {
        width = 8,
        height = 8,
        maxTime = 7
    }
}

local freezerTexture = "PrismaticHelper/temperatureBlocks/icepack"

function freezer.sprite(room, entity)
    local x, y, width, height = entity.x or 0, entity.y or 0, entity.width or 0, entity.height or 0

    local ninePatch = drawableNinePatch.fromTexture(freezerTexture, ninePatchOptions, x, y, width, height)

    return ninePatch:getDrawableSprite()
end

return { heater, freezer }