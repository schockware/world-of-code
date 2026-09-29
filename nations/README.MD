We are going to use `../domains/{domain}` and `../populations/{population}` to start building out specifications inside `./nations/{nation}/{domain}` folder.

For solutions that handle population interaction, we will assume the population is handled by whatever framework standard it already it.

For example, in `MVC` Razor or Blazor, React, or Vue, we do not need to include any reference to a population in the code since it is assumed the presentation layer handles interactions with `populations/humans`. 

An MCP server is assumed to only have `populations/ai` interactions.

Serverless Functions are assumed to only have `populations/automations` and so forth.

We would include a reference to the populations it supports in the documentation or `README.MD` files.

In cases of class libraries or isolated business layers, we assume there is no population (in fact it's an anti-pattern if we are leaking population details at that level).