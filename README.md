!\[Logo](Common%20Assets/non-binary\_flag.svg)



\# non-binary



This project doesn't have any pronouns, could possibly have a gluten intolerance though.



non-binary isn't inclusive of all, in fact it is deliberately designed for discrimination.



The Human Rights Commission might list a bunch of protected attributes that we cannot discriminate against, but not having a valid code signature ain't one of them.



Binary lives don't matter, and this is most assuredly ain't a safe space.



This project is a policy orchestration tool for App Control for Business (ACfB) formerly known as Windows Defender Application Control (WDAC) because you know... going 15 seconds without renaming something is an impossible feat for Microsoft.



While GUIs exist to build .cip files, seems like you have to use Intune to deploy them, and when you just want to allow one application for one device, it becomes thoroughly gay managing different policies and having to faf about with all this shit in Intune. Commercial products exist, but getting financially raped by cyber bros is... undesirable... so I have decided to build my own platform to achieve what I want to achieve.



The main goal is to enable MSPs or the smaller end of town to quickly and easily throw up this tool in a docker container and start making endpoints meet the ACSC E8 Application Control criterion for one of the desired maturity levels. Essentially really simplified version of AirLock/Threatlocker without all the flash stuff like ringfencing and probably not real time requesting of applications.... dunno... maybe in a v2 I could consider it but for now my goal is to ship out agents that will receive signed .cip files and load them into WDAC... I mean ACfB... and then any of the event logs entries for Code Integrity violations will be shipped by the agent back to the dashboard so there will be a central point of logs as required by the E8. Personally I like to use elasticsearch for centralised log ingestion so it is likely I will be building log shipping into elastic too.



99.99% of this code is made by SuperGrok AI, because Elon Musk hates pronouns. The remaining 0.01% of code comes from my cat periodically taking a stroll across the keyboard.



