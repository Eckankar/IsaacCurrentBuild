# Isaac Current Build

A simple little thing that extracts the following information from the currently on-going Binding of Isaac: Rebirth game:

* Seed
* Obtained items

Might be useful for making chatbots for people streaming Binding of Isaac: Rebirth, for instance.

# Installation

Download the source code and compile it with Visual Studio, or download the binary file.

# Known limitations

* The list of obtained items breaks if the build is rerolled. Sadly the game
  doesn't log that a reroll occurs, so it's not easy to properly update based on
  that.
