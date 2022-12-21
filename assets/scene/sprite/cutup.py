import os
from PIL import Image

with Image.open("projetilNew.png") as im:
    out = Image.new("RGBA", (256*7, 64))
    for i in range(0,7):
        box = (0, i*64, 256, i*64 + 64)
        region = im.crop(box)
        out.paste(region, (i*256, 0) )
    out.save("ProjectileArranged.png")
