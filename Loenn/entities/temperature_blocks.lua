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

local ninePatchOptions2nd = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local steam = {}

steam.name = "PrismaticHelper/SteamBlock"
steam.placements = {
    name = "steam",
    data = {
        width = 8,
        height = 8
    }
}

local steamTexture = "PrismaticHelper/temperatureBlocks/steam/steamblock01"

function steam.sprite(room, entity)
    local x, y, width, height = entity.x or 0, entity.y or 0, entity.width or 0, entity.height or 0
    return drawableNinePatch.fromTexture(steamTexture, ninePatchOptions2nd, x, y, width, height):getDrawableSprite()
end

local ice = {}

ice.name = "PrismaticHelper/IceBlock"
ice.placements = {
    name = "ice",
    data = {
        width = 8,
        height = 8
    }
}

local iceTexture = "PrismaticHelper/temperatureBlocks/ice/iceblock01"

function ice.sprite(room, entity)
    local x, y, width, height = entity.x or 0, entity.y or 0, entity.width or 0, entity.height or 0
    return drawableNinePatch.fromTexture(iceTexture, ninePatchOptions2nd, x, y, width, height):getDrawableSprite()
end

return { heater, freezer, steam, ice }