// Manual Jest mock for uuid — returns deterministic IDs in tests
let counter = 0;

module.exports = {
  v4: () => `test-uuid-${++counter}`,
};

// Reset counter between test files
beforeEach(() => {
  counter = 0;
});
