# Mars Explorer
Uses Oculus Rift, Thalmic Myo and Unity to allow you to fly around Mars.  My Hack Princeton Spring 2015 entry.

This repository is currently fairly large.  This is mostly due to my inclusion of some of the satellite imagery that I used.  I will try to come up with a better way of handling this in the future.

## Scenes:
I have included the three scenes that I used during development.

### Mars Box
The first thing I've really made with Unity... Please don't judge.  It's just a box with images of Mars mapped to the sides for the user to fly in.  I just used this to figure out flying with the Rift.

### Terrain
I tried to generate a terrain directly from an image here.  It worked decently well, but seemed laggy.  Note that the peaks are really an issue with the image, not with the terrains in Unity.

### Mars
This is what I demoed.  There are several tessellated planes that are the landscape.  The first is just some cobble stone that I was playing around with to ge tthe hang of things.  The other two planes are from two different satellites.  If you look at the included images, one is a heat map.  I was not able to figure out the proper linearization of this high resolution image at the hackathon - this is forthcoming.

I used a tessellation shader from Unity's documentation for this, all I did was modify the distance at which the detail of the landscape is increased to be much farther away.  The draw distance could use some optimization - turn it down if you are having performance issues.

## Notes:
This initial commit contains almost exactly what I submitted to Hack Princeton.  It is messy. I plan to tidy it up and implement the other features I mentioned at the competition later.
