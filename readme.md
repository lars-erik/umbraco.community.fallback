## Umbraco Community Fallback

A universal fallback data type that adapts other data types.  
Do your editors have to copy and paste a lot for listing and SEO texts?  
Do they often forget to add an image so the page layout is messed up?  
Fret not, here's the package to solve all your needs.

Here's a short demo:

[Umbraco Community Fallback Property - First Glance](https://www.youtube.com/watch?v=01oiT-3QhBg)

### TODOs

- Find empty cases
- Make extension points for empty cases
- Limit choices to compatible data types
- Limiting strategy for SEO title for instance. (Substr 160)
- Extendable transformation strategies (other datatypes than text)
- Prevent stack overflow, browser freezes and crashes (!)  
  (It is currently possible to fall back to "self" or subsequently through "self")


### Credits

A lot of this code is based on

* [@lottepitcher](https://github.com/lottepitcher)'s code in [Admin Only Property](https://github.com/LottePitcher/umbraco-admin-only-property) which totally sparked this idea!
* [@leekelleher](https://github.com/leekelleher)'s code in [Contentment]() and Adnim Only which totally powers 50%+ of this package.

Honorable mention for inspiration, motivation and a brief code flirt goes to

* [@deanleigh](https://github.com/deanleigh) et. al. for [Wholething Fallback Text Property](https://github.com/wholething/wholething-fallback-text-property).
