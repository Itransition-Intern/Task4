{
  description = "Task #4 ASP.NET Core MVC application";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  };

  outputs = { self, nixpkgs }:
    let
      system = "x86_64-linux";

      pkgs = import nixpkgs {
        inherit system;
      };

      dotnet = with pkgs.dotnetCorePackages; combinePackages [
        sdk_10_0
      ];
    in
    {
      devShells.${system}.default = pkgs.mkShell {
        packages = [
          dotnet
          pkgs.dotnet-ef
          pkgs.postgresql
        ];

        shellHook = ''
          export DOTNET_ROOT="${dotnet}"

          echo "======================================"
          echo " Task4 development environment"
          echo "======================================"
          echo "dotnet: $(dotnet --version)"
          echo "ef:     $(dotnet ef --version)"
          echo "psql:   $(psql --version)"
          echo
        '';
      };
    };
}