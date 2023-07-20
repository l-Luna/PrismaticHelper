local drawableSprite = require("structs.drawable_sprite")

local npp = {}

npp.specs = {}
npp.specs.Basic = {
    TopLeft = { { 0, 0 } },
    Top = { { 8, 0 } },
    TopRight = { { 16, 0 } },
    Right = { { 16, 8 } },
    BottomRight = { { 16, 16 } },
    Bottom = { { 8, 18 } },
    BottomLeft = { { 0, 16 } },
    Left = { { 0, 8 } },
    Inner = { { 8, 8 } }
}
npp.specs.CassetteLike = {
    TopLeft = { { 0, 0 } },
    Top = { { 8, 0 } },
    TopRight = { { 16, 0 } },
    Right = { { 16, 8 } },
    BottomRight = { { 16, 16 } },
    Bottom = { { 8, 18 } },
    BottomLeft = { { 0, 16 } },
    Left = { { 0, 8 } },
    Inner = { { 8, 8 } },

    InnerUR = { { 24, 0 } },
    InnerUL = { { 24, 8 } },
    InnerDR = { { 24, 16 } },
    InnerDL = { { 24, 24 } }
}
npp.specs.Extended = {
    TopLeft = { { 8, 8 }, { 8, 0 }, { 0, 8 } },
    TopRight = { { 48, 8 }, { 48, 0 }, { 56, 8 } },
    BottomLeft = { { 8, 48 }, { 0, 48 }, { 8, 56 } },
    BottomRight = { { 48, 48 }, { 56, 48 }, { 48, 56 } },
    InnerUL = { { 0, 0 } },
    InnerUR = { { 56, 0 } },
    InnerDL = { { 0, 56 } },
    InnerDR = { { 56, 56 } },
    Top = { { 16, 8 }, { 24, 8 }, { 32, 8 }, { 40, 8 } },
    Bottom = { { 16, 48 }, { 24, 48 }, { 32, 48 }, { 40, 48 } },
    Left = { { 8, 16 }, { 8, 24 }, { 8, 32 }, { 8, 40 } },
    Right = { { 48, 16 }, { 48, 24 }, { 48, 32 }, { 48, 40 } },
    Inner = (function()
        local r = {}
        for x = 0, 4 do
            for y = 0, 4 do
                table.insert(r, { 16 + x * 8, 16 + y * 8 })
            end
        end
        return r
    end)()
}

---@param spec table<string, table<number, table<number, number>>>
---@param texture string
function npp.drawableSprite(spec, texture, x, y, width, height)
    -- TODO: connections
    local ret = {}
    
    width = width / 8
    height = height / 8

    for i = 1, width do
        for j = 1, height do
            local left = i ~= 1
            local right = i ~= width
            local up = j ~= 1
            local down = j ~= height
            
            local subsprite = drawableSprite.fromTexture(texture, {
                x = x + (i - 1) * 8,
                y = y + (j - 1) * 8
            })

            if left and right and up and down then
                subsprite:useRelativeQuad(spec["Inner"][1][1], spec["Inner"][1][2], 8, 8, true, true)
            elseif left and right and (not up) and down then -- top of block
                subsprite:useRelativeQuad(spec["Top"][1][1], spec["Top"][1][2], 8, 8, true, true)
            elseif left and right and up and (not down) then -- bottom of block
                subsprite:useRelativeQuad(spec["Bottom"][1][1], spec["Bottom"][1][2], 8, 8, true, true)
            elseif left and (not right) and up and down then -- right of block
                subsprite:useRelativeQuad(spec["Right"][1][1], spec["Right"][1][2], 8, 8, true, true)
            elseif (not left) and right and up and down then -- left of block
                subsprite:useRelativeQuad(spec["Left"][1][1], spec["Left"][1][2], 8, 8, true, true)
            elseif left and (not right) and (not up) and down then -- up-right corner
                subsprite:useRelativeQuad(spec["TopRight"][1][1], spec["TopRight"][1][2], 8, 8, true, true)
            elseif (not left) and right and (not up) and down then -- up-left corner
                subsprite:useRelativeQuad(spec["TopLeft"][1][1], spec["TopLeft"][1][2], 8, 8, true, true)
            elseif left and (not right) and up and (not down) then -- down-right corner
                subsprite:useRelativeQuad(spec["BottomRight"][1][1], spec["BottomRight"][1][2], 8, 8, true, true)
            elseif (not left) and right and up and (not down) then -- down-left corner
                subsprite:useRelativeQuad(spec["BottomLeft"][1][1], spec["BottomLeft"][1][2], 8, 8, true, true)
            end
            
            table.insert(ret, subsprite)
        end
    end
    
    return ret
end

return npp