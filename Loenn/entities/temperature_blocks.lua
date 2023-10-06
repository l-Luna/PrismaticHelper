local drawableNinePatch = require("structs.drawable_nine_patch")
local ninepatchPlusPlus = require("mods").requireFromPlugin("libraries.ninepatch_plusplus")

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local heater = {}

heater.name = "PrismaticHelper/Heater"
heater.minimumSize = {16, 16}
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
freezer.minimumSize = {16, 16}
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
steam.minimumSize = {16, 16}
steam.placements = {
    name = "steam",
    data = {
        width = 8,
        height = 8,
        movesWithWind = false
    }
}

local steamTexture = "PrismaticHelper/temperatureBlocks/steam/steamblock01"

function steam.sprite(room, entity)
    local x, y, width, height = entity.x or 0, entity.y or 0, entity.width or 0, entity.height or 0
    return ninepatchPlusPlus.drawableSprite(ninepatchPlusPlus.specs.Extended, steamTexture, x, y, width, height)
end

local ice = {}

ice.name = "PrismaticHelper/IceBlock"
ice.minimumSize = {16, 16}
ice.placements = {
    name = "ice",
    data = {
        width = 8,
        height = 8,
        movesWithWind = false
    }
}

local iceTexture = "PrismaticHelper/temperatureBlocks/ice/iceblock01"

function ice.sprite(room, entity)
    local x, y, width, height = entity.x or 0, entity.y or 0, entity.width or 0, entity.height or 0
    return ninepatchPlusPlus.drawableSprite(ninepatchPlusPlus.specs.Extended, iceTexture, x, y, width, height)
end

return { heater, freezer, steam, ice }