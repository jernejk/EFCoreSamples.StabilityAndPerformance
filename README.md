# EFCoreSamples.StabilityAndPerformance
Showing bad and good examples for EF Core that impacts stability and performance of the applications

I have worked on quite a few projects and have seen a lot of EF Core queries.
On those projects it's not uncommon to have developers that complains about EF Core performance.
In a lot of cases, the problem is how the queries are written. :)

In this repo, I'm trying to show common mistakes I have found and some tricks on how to get better performance without writing much code.
Of course, I'll include some optimizations where more code is required.

The idea is to learn patterns that gives you *free performance with as little work as possible and applying more advance pattern when performance issue is identified.

We should never permaturerly optimize software however, that does mean we should not implement easy performance gains. (you can look at them as tech debt more than premature optimizations)

DB Source: https://www.sqlskills.com/sql-server-resources/sql-server-demos/

I used this DB because it's simple but has enough data to demonstrate why certain patterns are very dangerous and you should always use more optimum patterns. (which sometimes results in less code and better redability on top of better performance)
