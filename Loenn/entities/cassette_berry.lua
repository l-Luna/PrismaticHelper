local drawableSprite = require("structs.drawable_sprite")

local presentableColorNames = {
    ["Blue"] = 0,
    ["Rose"] = 1,
    ["Bright Sun"] = 2,
    ["Malachite"] = 3
}

local spriteColorNames = {
    "blue", "rose", "sun", "malachite"
}

local cassetteBerry = {}

cassetteBerry.name = "PrismaticHelper/CassetteBerry"
cassetteBerry.depth = -100
cassetteBerry.nodeLineRenderType = "fan"
cassetteBerry.nodeLimits = {0, -1}
cassetteBerry.placements = {
    {
        name = "normal",
        data = {
            winged = false,
            index = 0,
            distracted = false
        }
    },
    {
        name = "winged",
        data = {
            winged = true,
            index = 0,
            distracted = false
        }
    },
    {
        name = "distracted",
        data = {
            winged = true,
            index = 0,
            distracted = true
        }
    }
}

cassetteBerry.fieldInformation = {
    index = {
        fieldType = "integer",
        options = presentableColorNames,
        editable = false
    }
}

local texBase = "PrismaticHelper/collectables/cassette_berries/cas_berry_"

function cassetteBerry.sprite(room, entity)
    local tbl = {}
    
    local index = entity.index or 0
    local suffix = spriteColorNames[index + 1] or spriteColorNames[1]

    local sprite = drawableSprite.fromTexture(texBase .. suffix .. (entity.winged and "/wings00" or "/normal00"), entity)
    sprite:setJustification(0.5, 0.5)
    table.insert(tbl, sprite)

    if entity.winged and entity.distracted then
        local headphones = drawableSprite.fromTexture("PrismaticHelper/collectables/cassette_berries/cas_headphones/headphones00", entity)
        headphones:setJustification(0.5, 0.5)
        table.insert(tbl, headphones)
    end

    return tbl
end

function cassetteBerry.nodeTexture(room, entity)
    local hasNodes = entity.nodes and #entity.nodes > 0

    if hasNodes then
        return "collectables/strawberry/seed00"
    end
end

return cassetteBerry