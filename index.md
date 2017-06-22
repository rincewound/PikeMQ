# Pike MQ
Lightweight Message Qeueuing

## Intro
## Building Pike MQ
The PikeMQ reference implementation are two .Net Core libraries.

## Server Use

## Client Use


## Limitiations
- The broker uses asynchronous calls everywhere, so most work is done using the threadpool. Under heavy load the threadpool might get exhausted resulting in slowly degrading performance, as frames need longer to get a thread assigned.
- All subscriptions only last as long as the connections lasts.
- Due to the parallel and asynchronous nature of the broker there is no guarantee as to the order in which messages arrive, that are sent with QoS 0. QoS 1 and QoS 2 are guaranteed to arrive in order, if the client waits for the result of the message to arrive.

# Licence
None yet.


## Welcome to GitHub Pages

You can use the [editor on GitHub](https://github.com/rincewound/PikeMQ/edit/master/index.md) to maintain and preview the content for your website in Markdown files.

Whenever you commit to this repository, GitHub Pages will run [Jekyll](https://jekyllrb.com/) to rebuild the pages in your site, from the content in your Markdown files.

### Markdown

Markdown is a lightweight and easy-to-use syntax for styling your writing. It includes conventions for

```markdown
Syntax highlighted code block

# Header 1
## Header 2
### Header 3

- Bulleted
- List

1. Numbered
2. List

**Bold** and _Italic_ and `Code` text

[Link](url) and ![Image](src)
```

For more details see [GitHub Flavored Markdown](https://guides.github.com/features/mastering-markdown/).

### Jekyll Themes

Your Pages site will use the layout and styles from the Jekyll theme you have selected in your [repository settings](https://github.com/rincewound/PikeMQ/settings). The name of this theme is saved in the Jekyll `_config.yml` configuration file.

### Support or Contact

Having trouble with Pages? Check out our [documentation](https://help.github.com/categories/github-pages-basics/) or [contact support](https://github.com/contact) and we’ll help you sort it out.
