The main steps of the release process are:

- [ ] Develop/Build/Test the release candidate
- [ ] Prepare a release candidate (see below)
- [ ] Send the RC out to testers via Steam (see below)
- [ ] Begin work on a new dev release in the dev branch
- [ ] Create the supporting materials (see below)
- [ ] If any breaking bugs are found fix them in a new branch off main and merge them, go back to step 2
- [ ] When release day arrives, Create the release (see below)
- [ ] Release (see below)

### Prepare a Release Candidate

- [ ] Check the dev branch is fully up to date and pushed (`git status`)
- [ ] `git checkout main`
- [ ] `git pull`
- [ ] `git merge dev`
- [ ] `git push`
- [ ] `git tag RC_MM_DD_YY`
- [ ] `git push origin RC_MM_DD_YY`
- [ ] Create a build from main and send it out to testers
- [ ] [OPTIONAL] Address any breaking bugs in a branch off main
- [ ] [OPTIONAL] Merge any changes into main and dev
- [ ] [OPTIONAL] repeat this section from `git tag ...`

### Create supporting materials

- [ ] Write the changelog and announce post
- [ ] Update the roadmap
- [ ] Create new GIFs and Videos
- [ ] Add to git main
- [ ] Ensure there are no changes other than the GIFs and Videos
- [ ] `git status`
- [ ] `git add .`
- [ ] `git commit -m "Marketing materials for release"`
- [ ] `git push`

### prepare a release

- [ ] `git checkout main`
- [ ] `git tag Release_VERSIONNUMBER`
- [ ] `git push origin Release_VERSIONNUMBER`

### Release

- [ ] Create a build from main
- [ ] Push to itch and steam
- [ ] Update itch and steam pages
- [ ] Post announcement to itch, steam, discord, GitHub and mailing list