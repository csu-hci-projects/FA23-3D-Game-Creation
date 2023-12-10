git lfs uninstall
git filter-branch -f --prune-empty --tree-filter '
  git lfs checkout
  git lfs ls-files | cut -d " " -f 3 | xargs touch
  git rm -f .gitattributes
  git lfs ls-files | cut -d " " -f 3 | git add
' --tag-name-filter cat -- --all